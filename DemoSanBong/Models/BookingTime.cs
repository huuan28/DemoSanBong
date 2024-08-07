﻿namespace DemoSanBong.Models
{
    public class BookingTime
    {
        public int open { get; set; }
        public int close { get; set; }

        public List<DateTime> SelectBegin(DateTime date)
        {
            int begin = (date.Date == DateTime.Today && DateTime.Now.Hour > open) ? (DateTime.Now.Hour + 1) : open;
            var list = new List<DateTime>();
            for (var i = begin; i < close; i++)
            {
                list.Add(date.AddHours(i));
            }
            return list;
        }
        //public List<DateTime> SelectEnd(DateTime date)
        //{
        //    int begin = (date.Date == DateTime.Today && DateTime.Now.Hour < open) ? open : DateTime.Now.Hour+1;
        //    var list = new List<DateTime>();
        //    for (var i = begin+1; i <= 22; i++)
        //    {
        //        list.Add(date.AddHours(i));
        //    }
        //    return list;
        //}
        public List<DateTime> SelectEnd(DateTime beginTime)
        {
            var list = new List<DateTime>();
            var time = beginTime.Hour == 0 ? DateTime.Now.Hour : beginTime.Hour;
            for (var i = 1; i <= close - time; i++)
            {
                list.Add(beginTime.AddHours(i));
            }
            return list;
        }
        public List<DateTime> GetDays()
        {
            var list = new List<DateTime>();
            if (DateTime.Now.Hour < 21)
                list.Add(DateTime.Today);
            list.Add(DateTime.Today.AddDays(1));
            list.Add(DateTime.Today.AddDays(2));
            return list;
        }
        public BookingTime(Parameter rules)
        {
            open = rules.OpenTime;

            close = rules.CloseTime;
        }
    }
}
