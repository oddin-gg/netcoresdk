using System.Collections.Generic;
using System.Xml.Serialization;

namespace Oddin.Oddin.SDK.API.Entities
{
    [XmlRoot("producers")]
    public class ProducersDTO
    {
        [XmlElement("response_code")]
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
        [XmlElement("id")]
        public int Id { get; set; }

        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("description")]
        public string Description { get; set; }

        [XmlElement("api_url")]
        public string ApiUrl { get; set; }

        [XmlElement("active")]
        public bool IsActive { get; set; }

        [XmlElement("scope")]
        public string Scope { get; set; }

        [XmlElement("stateful_recovery_window_in_minutes")]
        public int RecoveryWindowMinutes { get; set; }
    }
}
