using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;


namespace CyyService.Model
{
    /// <summary>
    /// 区.
    /// </summary>
    public class District
    {
        /// <summary>
        /// 区名.
        /// </summary>
        [XmlAttribute("name")]
        public string Name { set; get; }
    }
}
