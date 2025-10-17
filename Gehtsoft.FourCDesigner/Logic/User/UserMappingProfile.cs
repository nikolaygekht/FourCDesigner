using AutoMapper;

namespace Gehtsoft.FourCDesigner.Logic.User;

/// <summary>
/// AutoMapper profile for User entity to UserInfo DTO mapping.
/// </summary>
public class UserMappingProfile : Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserMappingProfile"/> class.
    /// </summary>
    public UserMappingProfile()
    {
        CreateMap<Entities.User, UserInfo>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role))
            .ForMember(dest => dest.ActiveUser, opt => opt.MapFrom(src => src.ActiveUser));
    }
}
