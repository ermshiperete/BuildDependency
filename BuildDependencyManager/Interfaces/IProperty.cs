// Copyright (c) 2015 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)

namespace BuildDependency.Interfaces
{
	public interface IProperty
	{
		string Name { get; set; }
		string Value { get; set; }
	}
}