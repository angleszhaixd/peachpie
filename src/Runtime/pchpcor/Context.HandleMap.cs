﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pchp.Core
{
    partial class Context
    {
        /// <summary>
        /// Map of symbols declared within the context.
        /// </summary>
        /// <typeparam name="THandle">Symbol descriptor.</typeparam>
        /// <typeparam name="TComparerFactory">
        /// Factory providing string comparison object used to map symbol names.
        /// Expecting to get OrdinalIgnoreCase or Ordinal comparer.
        /// </typeparam>
        class HandleMap<THandle, TComparerFactory> where TComparerFactory : Utilities.IProvider<IEqualityComparer<string>>, new()
        {
            /// <summary>
            /// Map of referenced symbols.
            /// Initialized lazily, applies for the whole application and all the contexts.
            /// </summary>
            static Dictionary<string, THandle[]> _referencedSymbols;

            /// <summary>
            /// Names mapped to <see cref="_runtimeSymbols"/> indexes.
            /// </summary>
            static Dictionary<string, int> _nameMap;

            /// <summary>
            /// Symbols declared in runtime. Indexes correspond to the <see cref="_nameMap"/>.
            /// </summary>
            THandle[] _runtimeSymbols;

            static HandleMap()
            {
                var comparer = (new TComparerFactory()).Create();

                _referencedSymbols = new Dictionary<string, THandle[]>(comparer);
                _nameMap = new Dictionary<string, int>(comparer);
            }

            /// <summary>
            /// Initializes instance of the handle map to be used within a context.
            /// </summary>
            public HandleMap()
            {
                _runtimeSymbols = new THandle[_nameMap.Count];
            }

            public THandle[] TryGetHandle(string name)
            {
                // lookup app tables
                THandle[] handles;
                if (_referencedSymbols.TryGetValue(name, out handles))  // TODO: RW lock
                {
                    return handles;
                }

                // lookup context tables
                int index;
                if (_nameMap.TryGetValue(name, out index))
                {
                    var handle = _runtimeSymbols[index];
                    if (handle != null)
                    {
                        return new THandle[] { handle };
                    }
                }

                // nothing found
                return new THandle[0];
            }

            /// <summary>
            /// Declare handle within the runtime symbols table.
            /// </summary>
            /// <param name="index">Index variable corresponding to the name for faster declaration.</param>
            /// <param name="name">Symbol name.</param>
            /// <param name="handle">Handle of the symbol.</param>
            public void Declare(ref int index, string name, THandle handle)
            {
                Debug.Assert(!string.IsNullOrEmpty(name));

                EnsureIndex(ref index, name);
                EnsureSize(index);
                Declare(ref _runtimeSymbols[index], handle);
            }

            void Declare(ref THandle placeholder, THandle handle)
            {                
                // TODO: ErrCode when redefining
                //if (placeholder != null) { ... }
                
                placeholder = handle;
            }

            void EnsureSize(int index)
            {
                Debug.Assert(index >= 0);

                if (_runtimeSymbols.Length <= index)
                {
                    Array.Resize(ref _runtimeSymbols, (index + 1) * 2);
                }
            }

            /// <summary>
            /// Ensures given <paramref name="index"/> is initialized and associated with its <paramref name="name"/>.
            /// </summary>
            /// <param name="index">Index variable.</param>
            /// <param name="name">Associated symbol name.</param>
            public static void EnsureIndex(ref int index, string name)
            {
                if (index < 0)
                    index = GetIndex(name);
            }

            /// <summary>
            /// Gets index associated with given symbol name.
            /// </summary>
            static int GetIndex(string name)
            {
                int index;
                lock (_nameMap) // TODO: RW lock
                {
                    if (!_nameMap.TryGetValue(name, out index))
                    {
                        _nameMap[name] = index = _nameMap.Count;
                    }
                }

                //
                return index;
            }
        }
    }
}