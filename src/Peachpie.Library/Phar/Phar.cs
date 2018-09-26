﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Pchp.Core;
using Pchp.Core.QueryValue;
using Pchp.Library.Spl;

namespace Pchp.Library.Phar
{
    /// <summary>
    /// A high-level interface to accessing and creating phar archives.
    /// </summary>
    [PhpType(PhpTypeAttribute.InheritName)]
    public sealed class Phar : /*RecursiveDirectoryIterator,*/ Countable, ArrayAccess
    {
        #region .ctor

        public Phar(string fname, int flags = 0, string alias = default)
        {
            __construct(fname, flags, alias);
        }

        public void __construct(string fname, int flags = 0, string alias = default)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Countable, ArrayAccess

        public long count()
        {
            throw new NotImplementedException();
        }

        public bool offsetExists(PhpValue offset)
        {
            throw new NotImplementedException();
        }

        public PhpValue offsetGet(PhpValue offset)
        {
            throw new NotImplementedException();
        }

        public void offsetSet(PhpValue offset, PhpValue value)
        {
            throw new NotImplementedException();
        }

        public void offsetUnset(PhpValue offset)
        {
            throw new NotImplementedException();
        }

        #endregion

        /// <summary>
        /// Reads the currently executed file (a phar) and registers its manifest.
        /// </summary>
        /// <param name="ctx">Runtime context.</param>
        /// <param name="self">Current script.</param>
        /// <param name="alias">The alias that can be used in phar:// URLs to refer to this archive, rather than its full path.</param>
        /// <param name="dataoffset">Unused.</param>
        /// <returns>Always <c>true</c>.</returns>
        public static bool mapPhar(Context ctx, QueryValue<CallerScript> self, string alias, int dataoffset = 0)
        {
            Debugger.Break();

            var pharfile = PharExtensions.GetPharFile(self.Value.ScriptType);
            if (pharfile == null)
            {
                throw new PharException();
            }

            return PharExtensions.MapPhar(ctx, self.Value.ScriptType, alias);
        }

        public static string running(bool retphar = true)
        {
            // 
            throw new NotImplementedException();
        }

        public static bool canWrite() => false;

        public static bool loadPhar(string filename, string alias = default) => throw new NotSupportedException();

        public static void mount(string pharpath, string externalpath) => throw new NotSupportedException();

        public static void mungServer(PhpArray munglist) => throw new NotSupportedException();

        public static bool unlinkArchive(string archive) => throw new NotSupportedException();

        public static bool webPhar(string alias = default, string index = "index.php", string f404 = default, PhpArray mimetypes = default, IPhpCallable rewrites = default) => throw new NotSupportedException();
    }
}
