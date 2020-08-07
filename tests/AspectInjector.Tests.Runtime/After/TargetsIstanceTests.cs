using AspectInjector.Tests.Assets;
using AspectInjector.Tests.RuntimeAssets;

namespace AspectInjector.Tests.Runtime.After
{
    public class Instance_Tests : Global_Tests
    {
        protected override string Token => InstanceAspect.AfterExecuted;
    }
}