﻿using HlidacStatu.Util.Cache;
using System;

namespace HlidacStatu.Web.Framework
{

    public static class RemoteUrlFromWebCache
    {
        static RemoteUrlFromWebCache()
        {
        }

        public static volatile FileCacheManager Manager
                = FileCacheManager.GetSafeInstance("RemoteUrlFromWebCache",
                    urlfn => GetBinaryDataFromUrl(urlfn),
                    TimeSpan.FromHours(24*5));


        private static byte[] GetBinaryDataFromUrl(KeyAndId ki)
        {
            byte[] data = null;

            try
            {
                using (Devmasters.Net.Web.URLContent net = new Devmasters.Net.Web.URLContent(ki.ValueForData))
                {
                    net.Timeout = 7000;
                    net.IgnoreHttpErrors = true;
                    data = net.GetBinary().Binary;
                }
            }
            catch (Exception e)
            {
                HlidacStatu.Util.Consts.Logger.Error("Manager Save", e);
            }
            if (data == null || data.Length == 0)
                return System.IO.File.ReadAllBytes(HlidacStatu.Lib.Init.WebAppRoot + @"content\icons\largetile.png");
            else
                return data;

        }
    }
}