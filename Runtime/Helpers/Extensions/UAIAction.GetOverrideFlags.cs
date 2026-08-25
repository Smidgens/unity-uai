// // smidgens @ github
//
// namespace Smidgenomics.Unity.UAI
// {
// 	using System.Text;
// 	using System.Reflection;
//
// 	internal static partial class UAIAction_
// 	{
// 		public static EUAIActionOverrideFlags GetOverrideFlags(this UAIAction action)
// 		{
// 			var type = action.GetType();
// 			var flags = EUAIActionOverrideFlags.None;
// 			foreach (var (methodName, flag) in _methodFlags)
// 			{
// 				if (type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public)?.DeclaringType == type)
// 				{
// 					flags |= flag;
// 				}
// 			}
// 			return flags;
// 		}
//
// 		private static readonly (string, EUAIActionOverrideFlags)[] _methodFlags =
// 		{
// 			(nameof(UAIAction.ActivateAction), EUAIActionOverrideFlags.ActivateAction),
// 			(nameof(UAIAction.DeactivateAction), EUAIActionOverrideFlags.DeactivateAction),
// 			(nameof(UAIAction.GetActionCooldown), EUAIActionOverrideFlags.GetActionCooldown),
// 			(nameof(UAIAction.CanCancelAction), EUAIActionOverrideFlags.CanCancelAction),
// 			(nameof(UAIAction.GetWeightModifier), EUAIActionOverrideFlags.GetWeightModifier),
// 		};
//
// 	}
// }