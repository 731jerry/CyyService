using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;


namespace CyyService.Model
{

    [XmlRoot("address")]
    public class Address
    {


        /// <summary>
        /// 省列表.
        /// </summary>
        [XmlElement("province")]
        public List<Province> ProvinceList { set; get; }

        /// <summary>
        /// 根据名字， 查询省.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Province GetProvince(string name)
        {
            return ProvinceList.FirstOrDefault(p => p.Name == name);
        }

    }


}
