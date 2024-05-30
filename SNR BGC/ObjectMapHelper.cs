using AutoMapper;
using System.Collections.Generic;
using System.Linq;

namespace SNR_BGC
{
    public class ObjectMapHelper
    {
        public ObjectMapHelper()
        {
        }

        /// <summary>
        /// This helper method maps single object.
        /// </summary>
        /// <typeparam name="T">Source object type.</typeparam>
        /// <typeparam name="U">Target object type.</typeparam>
        /// <param name="model">Actual object to map.</param>
        /// <returns></returns>
        public static U MapObjectSingle<T, U>(T model)
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<T, U>());
            var mapper = new Mapper(config);

            return mapper.Map<U>(model);
        }

        /// <summary>
        /// This helper method maps list of objects.
        /// </summary>
        /// <typeparam name="T">Source object type.</typeparam>
        /// <typeparam name="U">Target object type.</typeparam>
        /// <param name="modelList">Actual object list to map.</param>
        /// <returns></returns>
        public static List<U> MapObjectList<T, U>(List<T> modelList)
        {
            var returnList = new List<U>();
            var config = new MapperConfiguration(cfg => cfg.CreateMap<T, U>());
            var mapper = new Mapper(config);

            if (modelList != null && modelList.Any())
                foreach (var item in modelList)
                    returnList.Add(mapper.Map<U>(item));

            return returnList;
        }
    }
}
