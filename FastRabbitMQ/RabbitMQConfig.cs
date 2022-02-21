using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Xml;

namespace FastRabbitMQ.Config
{
    internal class RabbitMQConfig : ConfigurationSection
    {

        #region 获取配置信息
        /// <summary>
        /// 获取配置信息
        /// </summary>
        /// <returns></returns>
        public static RabbitMQConfig GetConfig(string projectName = null, string dbFile = "web.config")
        {
            var section = new RabbitMQConfig();
            if (projectName == null)
            {
                if (string.Compare( dbFile, "web.config",false)==0 || string.Compare( dbFile, "app.config",false)==0)
                    section = (RabbitMQConfig)ConfigurationManager.GetSection("RabbitMQConfig");
                else
                {
                    var exeConfig = new ExeConfigurationFileMap();
                    exeConfig.ExeConfigFilename = string.Format("{0}bin\\{1}", AppDomain.CurrentDomain.BaseDirectory, dbFile);
                    section = (RabbitMQConfig)ConfigurationManager.OpenMappedExeConfiguration(exeConfig, ConfigurationUserLevel.None).GetSection("RabbitMQConfig");
                }
            }
            else
            {
                var assembly = Assembly.Load(projectName);
                using (var resource = assembly.GetManifestResourceStream(string.Format("{0}.{1}", projectName, dbFile)))
                {
                    if (resource != null)
                    {
                        using (var reader = new StreamReader(resource))
                        {
                            var content = reader.ReadToEnd();
                            var xmlDoc = new XmlDocument();
                            xmlDoc.LoadXml(content);
                            var nodelList = xmlDoc.SelectNodes("configuration/RabbitMQConfig");
                            foreach (XmlNode node in nodelList)
                            {
                                section.Host = node.Attributes["Host"].Value;
                                section.PassWord = node.Attributes["PassWord"].Value;
                                section.UserName = node.Attributes["UserName"].Value;
                                section.VirtualHost = node.Attributes["VirtualHost"].Value;
                                section.Port = int.Parse(node.Attributes["Port"].Value);
                            }
                        }
                    }
                }
            }

            return section;
        }
        #endregion


        #region 地址
        /// <summary>
        /// 地址
        /// </summary>
        [ConfigurationProperty("Host", IsRequired = true)]
        public string Host
        {
            get
            {
                return base["Host"].ToString();
            }
            set
            {
                base["Host"] = value;
            }
        }
        #endregion

        #region 用户名
        /// <summary>
        /// 用户名
        /// </summary>
        [ConfigurationProperty("UserName", IsRequired = true)]
        public string UserName
        {
            get
            {
                return base["UserName"].ToString();
            }
            set
            {
                base["UserName"] = value;
            }
        }
        #endregion

        #region 密码
        /// <summary>
        /// 密码
        /// </summary>
        [ConfigurationProperty("PassWord", IsRequired = true)]
        public string PassWord
        {
            get
            {
                return base["PassWord"].ToString();
            }
            set
            {
                base["PassWord"] = value;
            }
        }
        #endregion

        #region 端口
        /// <summary>
        /// 端口
        /// </summary>
        [ConfigurationProperty("Port", IsRequired = false, DefaultValue = 5672)]
        public int Port
        {
            get
            {
                return (int)base["Port"];
            }
            set
            {
                base["Port"] = value;
            }
        }
        #endregion

        #region Virtual Host
        /// <summary>
        /// Virtual Host
        /// </summary>
        [ConfigurationProperty("VirtualHost", IsRequired = false, DefaultValue = "/")]
        public string VirtualHost
        {
            get
            {
                return base["VirtualHost"].ToString();
            }
            set
            {
                base["VirtualHost"] = value;
            }
        }
        #endregion
    }
}