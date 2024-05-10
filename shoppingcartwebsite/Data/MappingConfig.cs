using AutoMapper;
using shoppingcartwebsite.Models;
using shoppingcartwebsite.ViewModels;

namespace shoppingcartwebsite.Data
{
    public class MappingConfig
    {
        public static MapperConfiguration RegisterMaps()
        {
            var mappingConfig = new MapperConfiguration(config =>
            {
                config.CreateMap<User, RegistrationViewModel>().ReverseMap();
               

            });
            return mappingConfig;
        }
    }
}
