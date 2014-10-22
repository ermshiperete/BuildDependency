// Copyright (c) 2014 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;

namespace BuildDependencyManager.RestClasses
{
	public enum BuildTagType
	{
		lastSuccessful,
		lastPinned,
		lastFinished,
		buildNumber,
		buildTag
	}
}
