﻿// <auto-generated>
// Do not edit this file yourself!
//
// This code was generated by Paradox Shader Mixin Code Generator.
// To generate it yourself, please install SiliconStudio.Paradox.VisualStudio.Package .vsix
// and re-save the associated .pdxfx.
// </auto-generated>

using SiliconStudio.Core;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Shaders;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Graphics;


#line 1 "C:\Users\Daniel\Documents\Paradox Projects\Bacteriophage\Bacteriophage\Bacteriophage.Game\Effects\BacteriophageEffectMain.pdxfx"
using SiliconStudio.Paradox.Effects.Data;

#line 3
namespace Bacteriophage.Effects
{

    #line 5
    public partial class BacteriophageEffectMain  : IShaderMixinBuilder
    {
        public void Generate(ShaderMixinSourceTree mixin, ShaderMixinContext context)
        {

            #line 11
            context.Mixin(mixin, "ShaderBase");

            #line 12
            context.Mixin(mixin, "TransformationWAndVP");

            #line 13
            context.Mixin(mixin, "BRDFDiffuseBase");

            #line 14
            context.Mixin(mixin, "BRDFSpecularBase");

            #line 15
            context.Mixin(mixin, "AlbedoFlatShading");

            #line 17
            if (context.GetParam(MaterialParameters.AlbedoDiffuse) != null)

                {

                    #line 18
                    var __subMixin = new ShaderMixinSourceTree() { Parent = mixin };

                    #line 18
                    context.Mixin(__subMixin, context.GetParam(MaterialParameters.AlbedoDiffuse));
                    mixin.Mixin.AddComposition("albedoDiffuse", __subMixin.Mixin);
                }

            #line 20
            if (context.GetParam(MaterialParameters.HasSkinningPosition))
            {

                #line 22
                mixin.Mixin.AddMacro("SkinningMaxBones", context.GetParam(MaterialParameters.SkinningMaxBones));

                #line 23
                context.Mixin(mixin, "TransformationSkinning");
            }
        }

        [ModuleInitializer]
        internal static void __Initialize__()

        {
            ShaderMixinManager.Register("BacteriophageEffectMain", new BacteriophageEffectMain());
        }
    }
}
