// Copyright (c) 2015 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
namespace BuildDependency.Interfaces
{
	public interface IProject
	{
		string Name { get; set; }
		string Id { get; set; }
		string Href { get; set; }
	}
}