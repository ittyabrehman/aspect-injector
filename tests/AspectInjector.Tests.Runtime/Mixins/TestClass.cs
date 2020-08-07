using AspectInjector.Broker;
using AspectInjector.Tests.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AspectInjector.Tests.RuntimeAssets;

namespace AspectInjector.Tests.Runtime.Mixins
{   
    internal class TestClassWrapper<T1>
    {
        [InjectInstanceAspect]
        [InjectGlobalAspect]
        private class TestClass<T2>
        {

        }
    }
}