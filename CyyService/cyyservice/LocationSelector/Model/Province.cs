using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;



namespace CyyService.Model
{

    /// <summary>
    /// 省.
    /// </summary>
    public class Province
    {

        /// <summary>
        /// 名称.
        /// </summary>
        [XmlAttribute("name")]
        public string Name { set; get; }



        /// <summary>
        /// 市列表.
        /// </summary>
        [XmlElement("city")]
        public List<City> CityList { set; get; }



        /// <summary>
        /// 根据名字， 查询市.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public City GetCity(string name)
        {
            return CityList.FirstOrDefault(p => p.Name == name);
        }
    }
}
