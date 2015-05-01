using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace CyyService.Model
{

    public class City
    {

        /// <summary>
        /// 城市名.
        /// </summary>
        [XmlAttribute("name")]
        public string Name { set; get; }



        /// <summary>
        /// 区列表.
        /// </summary>
        [XmlElement("country")]
        public List<District> DistrictList { set; get; }



        /// <summary>
        /// 根据名字， 查询区.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public District GetDistrict(string name)
        {
            return DistrictList.FirstOrDefault(p => p.Name == name);
        }

    }
}
