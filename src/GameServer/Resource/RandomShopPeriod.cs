//using NeoNetsphere.Database.Game;
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace NeoNetsphere.Resource
//{
//    public class RandomShopPeriod
//    {
//        public string Group { get; set; }

//        public ItemPeriodType Type { get; set; }

//        public ItemPriceType PriceType { get; set; }

//        public uint Value { get; set; }

//        public uint Probability { get; set; }

//        public RandomShopGrade Grade { get; set; }

//        public RandomShopPeriod(RandomShopPeriodEntity entity)
//        {
//            Group = entity.Name;
//            Probability = (uint)entity.Probability;
//            Grade = (RandomShopGrade)entity.Grade;
//            //var shopPrice = service.ShopPrices.Values.Select(x => x.Prices.FirstOrDefault(y => y.Id == entity.ShopPriceId)).Max();
//            //var priceType = service.Prices.Values.First(x => x.Prices.Any(y => y.Id == entity.ShopPriceId)).PriceType;
//            //Type = shopPrice.PeriodType;
//            //Value = shopPrice.Period;
//            //PriceType = priceType;
//        }
//    }
//}
