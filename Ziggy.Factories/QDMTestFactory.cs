using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ziggy.Models;

namespace Ziggy.Factories
{
    public static class QDMTestFactory
    {
        private static Func<TaborArb2074, QDMTest> FactoryMethod { get; set; }
        public static void Initalize(Func<TaborArb2074, QDMTest> factoryMethod)
        {
            FactoryMethod = factoryMethod;
        }
        public static QDMTest Create(TaborArb2074 taborArb)
        {
            if (FactoryMethod != null)
                return FactoryMethod(taborArb);
            return new QDMTest(taborArb);
        }
    }
}
