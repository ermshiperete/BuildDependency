// Copyright (c) 2014 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;

namespace BuildDependency.TeamCity.RestClasses
{
	public class Project
	{
		//<project id="project43" name="Adapt It" href="/guestAuth/app/rest/7.0/projects/id:project43"/>
		public string Name { get; set; }
		public string Id { get; set; }
		public string Href { get; set; }

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			if (obj.GetType() != GetType())
				return false;
			return Equals((Project) obj);
		}

		private bool Equals(Project other)
		{
			return Id == other.Id;
		}

		public override int GetHashCode()
		{
			return Id.GetHashCode();
		}

		public override string ToString()
		{
			// WPF doesn't use Text (see https://github.com/picoe/Eto/issues/414), so we have to
			// return the desired value from ToString as well as Text.
			return Text;
			//return string.Format("[Project: Name={0}, Id={1}, Href={2}]", Name, Id, Href);
		}

		public string Text { get { return Name; }}
	}
}

