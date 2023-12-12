using Rhino.DocObjects;

namespace Crash.Handlers
{
	public static class RhinoObjectAttributesUtils
	{
		// TODO : Move
		internal static Dictionary<string, string> GetAttributeDifferencesAsDictionary(ObjectAttributes oldAttributes,
			ObjectAttributes newAttributes)
		{
			var results = new Dictionary<string, string>();
			if (!oldAttributes.Space.Equals(newAttributes.Space))
			{
				results.Add(nameof(ObjectAttributes.Space), newAttributes.Space.ToString());
			}

			if (!oldAttributes.Visible.Equals(newAttributes.Visible))
			{
				results.Add(nameof(ObjectAttributes.Visible),
				            newAttributes.Visible ? bool.TrueString : bool.FalseString);
			}

			if (oldAttributes.Name?.Equals(newAttributes.Name) == false)
			{
				results.Add(nameof(ObjectAttributes.Name), newAttributes.Name);
			}

			if (oldAttributes.Url?.Equals(newAttributes.Url) == false)
			{
				results.Add(nameof(ObjectAttributes.Url), newAttributes.Url);
			}

			if (!oldAttributes.LayerIndex.Equals(newAttributes.LayerIndex))
			{
				results.Add(nameof(ObjectAttributes.LayerIndex), newAttributes.LayerIndex.ToString());
			}

			return results;
			/* Currently Unsupported
			   if (!oldAttributes.CastsShadows.Equals(newAttributes.CastsShadows))
			   {
			   results.Add(nameof(ObjectAttributes.CastsShadows), newAttributes.CastsShadows);
			   }
			   if (!oldAttributes.ClipParticipationForAll.Equals(newAttributes.ClipParticipationForAll))
			   {
			   results.Add(nameof(ObjectAttributes.ClipParticipationForAll), newAttributes.ClipParticipationForAll);
			   }
			   if (!oldAttributes.ClipParticipationForNone.Equals(newAttributes.ClipParticipationForNone))
			   {
			   results.Add(nameof(ObjectAttributes.ClipParticipationForNone), newAttributes.ClipParticipationForNone);
			   }
			   if (!oldAttributes.HasMapping.Equals(newAttributes.HasMapping))
			   {
			   results.Add(nameof(ObjectAttributes.HasMapping), newAttributes.HasMapping);
			   }
			   if (!oldAttributes.HatchBoundaryVisible.Equals(newAttributes.HatchBoundaryVisible))
			   {
			   results.Add(nameof(ObjectAttributes.HatchBoundaryVisible), newAttributes.HatchBoundaryVisible);
			   }
			   if (!oldAttributes.ReceivesShadows.Equals(newAttributes.ReceivesShadows))
			   {
			   results.Add(nameof(ObjectAttributes.ReceivesShadows), newAttributes.ReceivesShadows);
			   }
			   if (!oldAttributes.HatchBackgroundFillColor.Equals(newAttributes.HatchBackgroundFillColor))
			   {
			   results.Add(nameof(ObjectAttributes.HatchBackgroundFillColor), newAttributes.HatchBackgroundFillColor);
			   }
			   if (!oldAttributes.ObjectColor.Equals(newAttributes.ObjectColor))
			   {
			   results.Add(nameof(ObjectAttributes.ObjectColor), newAttributes.ObjectColor);
			   }
			   if (!oldAttributes.PlotColor.Equals(newAttributes.PlotColor))
			   {
			   results.Add(nameof(ObjectAttributes.PlotColor), newAttributes.PlotColor);
			   }
			   if (!oldAttributes.Decals.Equals(newAttributes.Decals))
			   {
			   results.Add(nameof(ObjectAttributes.Decals), newAttributes.Decals);
			   }
			   if (!oldAttributes.LinetypePatternScale.Equals(newAttributes.LinetypePatternScale))
			   {
			   results.Add(nameof(ObjectAttributes.LinetypePatternScale), newAttributes.LinetypePatternScale);
			   }
			   if (!oldAttributes.PlotWeight.Equals(newAttributes.PlotWeight))
			   {
			   results.Add(nameof(ObjectAttributes.PlotWeight), newAttributes.PlotWeight);
			   }
			   if (!oldAttributes.SectionHatchRotationRadians.Equals(newAttributes.SectionHatchRotationRadians))
			   {
			   results.Add(nameof(ObjectAttributes.SectionHatchRotationRadians), newAttributes.SectionHatchRotationRadians);
			   }
			   if (!oldAttributes.SectionHatchScale.Equals(newAttributes.SectionHatchScale))
			   {
			   results.Add(nameof(ObjectAttributes.SectionHatchScale), newAttributes.SectionHatchScale);
			   }
			   if (!oldAttributes.ObjectFrameFlags.Equals(newAttributes.ObjectFrameFlags))
			   {
			   results.Add(nameof(ObjectAttributes.ObjectFrameFlags), newAttributes.ObjectFrameFlags);
			   }
			   if (!oldAttributes.File3dmMeshModifiers.Equals(newAttributes.File3dmMeshModifiers))
			   {
			   results.Add(nameof(ObjectAttributes.File3dmMeshModifiers), newAttributes.File3dmMeshModifiers);
			   }
			   if (!oldAttributes.ViewportId.Equals(newAttributes.ViewportId))
			   {
			   results.Add(nameof(ObjectAttributes.ViewportId), newAttributes.ViewportId);
			   }
			   if (!oldAttributes.DisplayOrder.Equals(newAttributes.DisplayOrder))
			   {
			   results.Add(nameof(ObjectAttributes.DisplayOrder), newAttributes.DisplayOrder);
			   }
			   if (!oldAttributes.GroupCount.Equals(newAttributes.GroupCount))
			   {
			   results.Add(nameof(ObjectAttributes.GroupCount), newAttributes.GroupCount);
			   }
			   if (!oldAttributes.LinetypeIndex.Equals(newAttributes.LinetypeIndex))
			   {
			   results.Add(nameof(ObjectAttributes.LinetypeIndex), newAttributes.LinetypeIndex);
			   }
			   if (!oldAttributes.MaterialIndex.Equals(newAttributes.MaterialIndex))
			   {
			   results.Add(nameof(ObjectAttributes.MaterialIndex), newAttributes.MaterialIndex);
			   }
			   if (!oldAttributes.SectionHatchIndex.Equals(newAttributes.SectionHatchIndex))
			   {
			   results.Add(nameof(ObjectAttributes.SectionHatchIndex), newAttributes.SectionHatchIndex);
			   }
			   if (!oldAttributes.WireDensity.Equals(newAttributes.WireDensity))
			   {
			   results.Add(nameof(ObjectAttributes.WireDensity), newAttributes.WireDensity);
			   }
			   if (!oldAttributes.MaterialRefs.Equals(newAttributes.MaterialRefs))
			   {
			   results.Add(nameof(ObjectAttributes.MaterialRefs), newAttributes.MaterialRefs);
			   }
			   if (!oldAttributes.CustomMeshingParameters.Equals(newAttributes.CustomMeshingParameters))
			   {
			   results.Add(nameof(ObjectAttributes.CustomMeshingParameters), newAttributes.CustomMeshingParameters);
			   }
			   if (!oldAttributes.ClipParticipationSource.Equals(newAttributes.ClipParticipationSource))
			   {
			   results.Add(nameof(ObjectAttributes.ClipParticipationSource), newAttributes.ClipParticipationSource);
			   }
			   if (!oldAttributes.ColorSource.Equals(newAttributes.ColorSource))
			   {
			   results.Add(nameof(ObjectAttributes.ColorSource), newAttributes.ColorSource);
			   }
			   if (!oldAttributes.ObjectDecoration.Equals(newAttributes.ObjectDecoration))
			   {
			   results.Add(nameof(ObjectAttributes.ObjectDecoration), newAttributes.ObjectDecoration);
			   }
			   if (!oldAttributes.LinetypeSource.Equals(newAttributes.LinetypeSource))
			   {
			   results.Add(nameof(ObjectAttributes.LinetypeSource), newAttributes.LinetypeSource);
			   }
			   if (!oldAttributes.MaterialSource.Equals(newAttributes.MaterialSource))
			   {
			   results.Add(nameof(ObjectAttributes.MaterialSource), newAttributes.MaterialSource);
			   }
			   if (!oldAttributes.Mode.Equals(newAttributes.Mode))
			   {
			   results.Add(nameof(ObjectAttributes.Mode), newAttributes.Mode);
			   }
			   if (!oldAttributes.PlotColorSource.Equals(newAttributes.PlotColorSource))
			   {
			   results.Add(nameof(ObjectAttributes.PlotColorSource), newAttributes.PlotColorSource);
			   }
			   if (!oldAttributes.PlotWeightSource.Equals(newAttributes.PlotWeightSource))
			   {
			   results.Add(nameof(ObjectAttributes.PlotWeightSource), newAttributes.PlotWeightSource);
			   }
			   if (!oldAttributes.SectionAttributesSource.Equals(newAttributes.SectionAttributesSource))
			   {
			   results.Add(nameof(ObjectAttributes.SectionAttributesSource), newAttributes.SectionAttributesSource);
			   }
			   if (!oldAttributes.SectionFillRule.Equals(newAttributes.SectionFillRule))
			   {
			   results.Add(nameof(ObjectAttributes.SectionFillRule), newAttributes.SectionFillRule);
			   }
			   if (!oldAttributes.RenderMaterial.Equals(newAttributes.RenderMaterial))
			   {
			   results.Add(nameof(ObjectAttributes.RenderMaterial), newAttributes.RenderMaterial);
			   }
			   if (!oldAttributes.OCSMappingChannelId.Equals(newAttributes.OCSMappingChannelId))
			   {
			   results.Add(nameof(ObjectAttributes.OCSMappingChannelId), newAttributes.OCSMappingChannelId);
			   }
			*/
		}
	}
}
