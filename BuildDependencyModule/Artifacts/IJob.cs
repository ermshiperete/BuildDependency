// Copyright (c) 2014 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;

namespace BuildDependency.Artifacts
{
	public interface IJob
	{
		Conditions Conditions { get; }
		bool Execute(ILog log, string workDir);
		string ToString();
	}
}
