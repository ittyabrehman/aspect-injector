using AspectInjector.Tests.Assets;
using AspectInjector.Tests.RuntimeAssets;

namespace AspectInjector.Tests.Runtime.Around
{
    public class Instance_Tests : Global_Tests
    {
        protected override string EnterToken => InstanceAspect.AroundEnter;
        protected override string ExitToken => InstanceAspect.AroundExit;
    }
}