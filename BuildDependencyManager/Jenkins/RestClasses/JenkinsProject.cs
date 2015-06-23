// Copyright (c) 2015 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using BuildDependency.Interfaces;

namespace BuildDependency.Jenkins.RestClasses
{
	class JenkinsProject: IProject
	{
		#region IProject Members

		public string Name { get; set; }
		public string Id { get; set; }
		public string Href { get; set; }

		#endregion

		public override bool Equals(object obj)
		{
			var other = obj as IProject;
			if (other == null)
				return false;
			return Name == other.Name && Id == other.Id && Href == other.Href;
		}

		public override string ToString()
		{
			return string.Format("[Id={0}, Name={1}, Href={2}]", Id, Name, Href);
		}

		public override int GetHashCode()
		{
			return Id.GetHashCode() ^ Name.GetHashCode() ^ Href.GetHashCode();
		}
	}
}
