using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Capstone.Models
{
    // Author Peter Szadurski
    [JsonObject(MemberSerialization.OptIn)]
    public class Hits
    {
        [JsonProperty(PropertyName = "recipe")]
        public RecipeModel Recipe { get; set; }
    }

    [DataContract]
    [Serializable]
    public class SerializableHits
    {
        [DataMember]
        public SerializableRecipeModel Recipe { get; set; }
        public SerializableHits(Hits m)
        {
            Recipe = new SerializableRecipeModel(m.Recipe);
        }
    }
    public class RecipeQueryModel
    {


        [JsonProperty(PropertyName = "q")]
        public string Query { get; set; }
        [JsonProperty(PropertyName = "hits")]

        public Hits[] Hits { get; set; }

    }
    [DataContract]
    [Serializable]
    public class SerializableRecipeQueryModel
    {
        [DataMember]
        public string Query { get; set; }
        [DataMember]
        SerializableHits[] Hits { get; set; }

        public SerializableRecipeQueryModel(RecipeQueryModel m)
        {
            Query = m.Query;
            List<SerializableHits> h = new List<SerializableHits>();
            for(int i = 0; i < m.Hits.Length; i++)
            {
                h.Add(new SerializableHits(m.Hits[i]));
            }
            Hits = h.ToArray();
        }
    }
}