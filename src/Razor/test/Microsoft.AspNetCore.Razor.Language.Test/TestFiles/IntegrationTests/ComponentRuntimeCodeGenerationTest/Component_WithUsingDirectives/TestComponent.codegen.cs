// <auto-generated/>
#pragma warning disable 1591
namespace Test
{
    #line hidden
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Components;
    using Test2;
    [Microsoft.AspNetCore.Components.RouteAttribute("/MyPage")]
    [Microsoft.AspNetCore.Components.RouteAttribute("/AnotherRoute/{id}")]
    public class TestComponent : Microsoft.AspNetCore.Components.ComponentBase
    {
        #pragma warning disable 1998
        protected override void BuildRenderTree(Microsoft.AspNetCore.Components.RenderTree.RenderTreeBuilder builder)
        {
            builder.OpenComponent<Test.MyComponent>(0);
            builder.CloseComponent();
            builder.AddMarkupContent(1, "\r\n");
            builder.OpenComponent<Test2.MyComponent2>(2);
            builder.CloseComponent();
        }
        #pragma warning restore 1998
    }
}
#pragma warning restore 1591
