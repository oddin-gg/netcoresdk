using System.Collections.Generic;
using System.Xml.Serialization;

namespace Oddin.Oddin.SDK.API.Entities
{
    [XmlRoot("producers")]
    public class ProducersDTO
    {
        [XmlAttribute("response_code")]
        public string ResponseCode { get; set; }

        [XmlElement("producer")]
        public List<ProducerDTO> Producers { get; set; }

        public ProducersDTO()
        {
            Producers = new List<ProducerDTO>();
        }
    }

    public class ProducerDTO
    {
        [XmlAttribute("id")]
        public int Id { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("description")]
        public string Description { get; set; }

        [XmlAttribute("api_url")]
        public string ApiUrl { get; set; }

        [XmlAttribute("active")]
        public bool IsActive { get; set; }

        [XmlAttribute("scope")]
        public string Scope { get; set; }

        [XmlAttribute("stateful_recovery_window_in_minutes")]
        public int RecoveryWindowMinutes { get; set; }
    }
}
