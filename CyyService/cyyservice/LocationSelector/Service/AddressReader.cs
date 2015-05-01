using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using CyyService.Model;


namespace CyyService.Service
{
    public class AddressReader
    {

        /// <summary>
        /// 从 XML 文件读取地址信息.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public Address ReadFromXmlFile(string fileName)
        {
            XmlSerializer xs = new XmlSerializer(typeof(Address));
            StreamReader sr = new StreamReader(fileName, Encoding.Default);
            Address result = xs.Deserialize(sr) as Address;
            sr.Close();
            return result;
        }

    }


}
