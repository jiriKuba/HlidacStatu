﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HlidacStatu.Lib.Data
{
    public partial class Search
    {
        public static int Limit = 50;
        public static int FirmyLimit = 1000;


        public class GeneralResult<T> : ISearchResult
        {
            public IEnumerable<T> Result { get; private set; }
            public long Total { get; private set; }
            public bool IsValid { get; private set; }
            public bool HasResult { get { return IsValid && Total > 0; } }
            public string DataSource { get; set; }
            public TimeSpan ElapsedTime { get; set; } = TimeSpan.Zero;

            public GeneralResult(IEnumerable<T> result)
                : this(result?.Count() ?? 0, result, true)
            { }

            public GeneralResult(long total, IEnumerable<T> result, bool isValid = true)
            {
                this.Result = result;
                this.Total = total;
                this.IsValid = isValid;
            }

        }


    }
}
