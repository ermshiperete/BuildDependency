// Copyright (c) 2014 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using System.Threading.Tasks;

namespace BuildDependency.Artifacts
{
	public interface IJob
	{
		Conditions Conditions { get; }
		Task<bool> Execute(ILog log, string workDir);
		string ToString();
	}
}
