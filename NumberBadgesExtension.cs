//
// NumberBadgesExtension.cs
//
// PintaNumberBadges - a Pinta add-in that stamps numbered circular badges.
//
// Licensed under the MIT License, see README.md.
//
// Pinta discovers this class through Mono.Addins: the [Extension] attribute
// registers it as an extension of Pinta.Core.IExtension (the add-in root's
// type extension point), and Pinta calls Initialize()/Uninitialize() as the
// add-in is loaded/unloaded.
//

using Pinta.Core;

namespace PintaNumberBadges;

[Mono.Addins.Extension]
public sealed class NumberBadgesExtension : IExtension
{
	public void Initialize ()
	{
		PintaCore.Tools.AddTool (new NumberBadgeTool (PintaCore.Services));
	}

	public void Uninitialize ()
	{
		PintaCore.Tools.RemoveInstanceOfTool<NumberBadgeTool> ();
	}
}
