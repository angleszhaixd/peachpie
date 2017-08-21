﻿using Pchp.Core;
using Pchp.Core.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Pchp.Library.Spl
{
    [PhpExtension(SplExtension.Name)]
    public static class SplObjects
    {
        /// <summary>Return hash id for given object.</summary>
        /// <param name="obj">Object instance. Cannot be <c>null</c>.</param>
        /// <returns>Object hash code as 32-digit hexadecimal number.</returns>
        public static string spl_object_hash(object obj)
        {
            if (obj != null)
            {
                return object_hash_internal_string(obj);
            }
            else
            {
                PhpException.InvalidArgumentType(nameof(obj), "object");
                return string.Empty;
            }
        }

        internal static string object_hash_internal_string(object obj)
        {
            return object_hash_internal(obj).ToString("x32");
        }

        internal static int object_hash_internal(object obj)
        {
            Debug.Assert(obj != null);
            return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
        }

        /// <summary>
        /// This function returns an array with the current available SPL classes.
        /// </summary>
        public static PhpArray spl_classes()
        {
            throw new NotImplementedException();
        }
    }
}
