﻿using System;
using System.Net;
using System.Threading.Tasks;

namespace DataLayer.BBCScheduleService
{
    public class Radio4Fm : IRadio4Fm
    {
        public enum ResponseTypes { Xml, Json}      

        private const string BbcRadio4Fm = "http://www.bbc.co.uk/radio4/programmes/schedules/fm/";

        public ResponseTypes ResponseType { get; set; }         //default is xml

        public async Task<string> GetSchedule(DateTime day)
        {
            var request = WebRequest.CreateHttp(FormUrl(day));
            var ws = await request.GetResponseAsync();
            return ws.ResponseUri.ToString();
        }

        private string FormUrl(DateTime day)
        {
            return string.Format("{0}/{1}/{2}/{3}.{4}", BbcRadio4Fm, day.Year, day.Month, day.Day, ResponseType.ToString().ToLowerInvariant());
        }
    }
}