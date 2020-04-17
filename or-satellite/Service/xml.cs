using System.Collections.Generic;
using System.Xml.Serialization;

namespace or_satellite.Service
{
[XmlRoot(ElementName="author", Namespace="http://www.w3.org/2005/Atom")]
	public class Author {
		[XmlElement(ElementName="name", Namespace="http://www.w3.org/2005/Atom")]
		public string Name { get; set; }
	}

	[XmlRoot(ElementName="Query", Namespace="http://a9.com/-/spec/opensearch/1.1/")]
	public class Query {
		[XmlAttribute(AttributeName="role")]
		public string Role { get; set; }
		[XmlAttribute(AttributeName="searchTerms")]
		public string SearchTerms { get; set; }
		[XmlAttribute(AttributeName="startPage")]
		public string StartPage { get; set; }
	}

	[XmlRoot(ElementName="link", Namespace="http://www.w3.org/2005/Atom")]
	public class Link {
		[XmlAttribute(AttributeName="rel")]
		public string Rel { get; set; }
		[XmlAttribute(AttributeName="type")]
		public string Type { get; set; }
		[XmlAttribute(AttributeName="href")]
		public string Href { get; set; }
	}

	[XmlRoot(ElementName="date", Namespace="http://www.w3.org/2005/Atom")]
	public class Date {
		[XmlAttribute(AttributeName="name")]
		public string Name { get; set; }
		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName="int", Namespace="http://www.w3.org/2005/Atom")]
	public class Int {
		[XmlAttribute(AttributeName="name")]
		public string Name { get; set; }
		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName="double", Namespace="http://www.w3.org/2005/Atom")]
	public class Double {
		[XmlAttribute(AttributeName="name")]
		public string Name { get; set; }
		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName="str", Namespace="http://www.w3.org/2005/Atom")]
	public class Str {
		[XmlAttribute(AttributeName="name")]
		public string Name { get; set; }
		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName="entry", Namespace="http://www.w3.org/2005/Atom")]
	public class Entry {
		[XmlElement(ElementName="title", Namespace="http://www.w3.org/2005/Atom")]
		public string Title { get; set; }
		[XmlElement(ElementName="link", Namespace="http://www.w3.org/2005/Atom")]
		public List<Link> Link { get; set; }
		[XmlElement(ElementName="id", Namespace="http://www.w3.org/2005/Atom")]
		public string Id { get; set; }
		[XmlElement(ElementName="summary", Namespace="http://www.w3.org/2005/Atom")]
		public string Summary { get; set; }
		[XmlElement(ElementName="date", Namespace="http://www.w3.org/2005/Atom")]
		public List<Date> Date { get; set; }
		[XmlElement(ElementName="int", Namespace="http://www.w3.org/2005/Atom")]
		public List<Int> Int { get; set; }
		[XmlElement(ElementName="double", Namespace="http://www.w3.org/2005/Atom")]
		public Double Double { get; set; }
		[XmlElement(ElementName="str", Namespace="http://www.w3.org/2005/Atom")]
		public List<Str> Str { get; set; }
	}

	[XmlRoot(ElementName="feed", Namespace="http://www.w3.org/2005/Atom")]
	public class Feed {
		[XmlElement(ElementName="title", Namespace="http://www.w3.org/2005/Atom")]
		public string Title { get; set; }
		[XmlElement(ElementName="subtitle", Namespace="http://www.w3.org/2005/Atom")]
		public string Subtitle { get; set; }
		[XmlElement(ElementName="updated", Namespace="http://www.w3.org/2005/Atom")]
		public string Updated { get; set; }
		[XmlElement(ElementName="author", Namespace="http://www.w3.org/2005/Atom")]
		public Author Author { get; set; }
		[XmlElement(ElementName="id", Namespace="http://www.w3.org/2005/Atom")]
		public string Id { get; set; }
		[XmlElement(ElementName="totalResults", Namespace="http://a9.com/-/spec/opensearch/1.1/")]
		public string TotalResults { get; set; }
		[XmlElement(ElementName="startIndex", Namespace="http://a9.com/-/spec/opensearch/1.1/")]
		public string StartIndex { get; set; }
		[XmlElement(ElementName="itemsPerPage", Namespace="http://a9.com/-/spec/opensearch/1.1/")]
		public string ItemsPerPage { get; set; }
		[XmlElement(ElementName="Query", Namespace="http://a9.com/-/spec/opensearch/1.1/")]
		public Query Query { get; set; }
		[XmlElement(ElementName="link", Namespace="http://www.w3.org/2005/Atom")]
		public List<Link> Link { get; set; }
		[XmlElement(ElementName="entry", Namespace="http://www.w3.org/2005/Atom")]
		public Entry Entry { get; set; }
		[XmlAttribute(AttributeName="opensearch", Namespace="http://www.w3.org/2000/xmlns/")]
		public string Opensearch { get; set; }
		[XmlAttribute(AttributeName="xmlns")]
		public string Xmlns { get; set; }
	}

}