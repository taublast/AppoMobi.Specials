using System;

namespace AppoMobi.Specials.Localization;

public class FromResources : Attribute

{
	public Type Type { get; set; }
}