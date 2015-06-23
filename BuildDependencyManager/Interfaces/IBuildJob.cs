// Copyright (c) 2015 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)

namespace BuildDependency.Interfaces
{
	public interface IBuildJob
	{
		string Id { get; }
		string Name { get; }
		string Url { get; }
		string Href { get; }
		string ProjectName { get; }
		string ProjectId { get; }
	}
}