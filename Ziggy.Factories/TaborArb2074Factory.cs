using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ziggy.Models;

namespace Ziggy.Factories
{
    public static class TaborArb2074Factory
    {
        private static Func<string, TaborArb2074> FactoryMethod { get; set; }
        public static void Initialize(Func<string, TaborArb2074> factoryMethod)
        {
            FactoryMethod = factoryMethod;
        }
        public static TaborArb2074 Create(string address)
        {
            if (FactoryMethod != null)
                return FactoryMethod(address);
            return new TaborArb2074(address);
        }
    }
}
