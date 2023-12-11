namespace Crash.Handlers.InternalEvents.Wrapping
{
	// TODO : Include Plane Cache or otherwise
	internal record TransformRecord : IUndoRedoCache
	{
		public readonly CrashTransformEventArgs TransformArgs;

		internal TransformRecord(CrashTransformEventArgs args)
		{
			TransformArgs = args;
		}

		public IUndoRedoCache GetInverse()
		{
			var transformCache = TransformArgs.Transform.ToRhino();
			transformCache.TryGetInverse(out var inverseTransform);

			var newArgs = new CrashTransformEventArgs(TransformArgs.Doc,
			                                          inverseTransform.ToCrash(),
			                                          TransformArgs.Objects,
			                                          TransformArgs.ObjectsWillBeCopied);
			return new TransformRecord(newArgs);
		}
	}
}
