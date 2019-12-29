using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 测试
{
    class Program
    {

        class Pet
        {
            public int id;
            public string name;
        }
        static void Main(string[] args)
        {
            Pet pet = new Pet
            {
                name = "哈哈哈",
                id = 1234
            };

            var petJson = JsonConvert.SerializeObject(pet);
            var pet1 = JsonConvert.DeserializeObject<Pet>(petJson);

            int i = 0;
        }
        
    }
}
