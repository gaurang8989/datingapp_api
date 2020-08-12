using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using WebApplication1.Data;
using WebApplication1.Dtos;
using WebApplication1.Models;

namespace WebApplication1.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<User, UserForListDto>()
                .ForMember(dest => dest.PhotoUrl, opt =>
                 opt.MapFrom(src => src.Photos.FirstOrDefault(p => p.IsMain).Url)) 
                .ForMember(dest => dest.Age, opt => 
                 opt.MapFrom(src => src.DateOfBirth.CalculateAge()));  //Individual Mapping
            CreateMap<User, UserForDetailDto>()
             .ForMember(dest => dest.PhotoUrl, opt =>
                opt.MapFrom(src => src.Photos.FirstOrDefault(p => p.IsMain).Url))
                 .ForMember(dest => dest.Age, opt =>
                 opt.MapFrom(src => src.DateOfBirth.CalculateAge()));  //Individual Mapping

            CreateMap<Photo, PhotosForDetailedDto>();
            CreateMap<UserForUpdateDto, User>();
            CreateMap<Photo, PhotoForReturnDto>();  //PhotosController Mapping id name....
            CreateMap<PhotoForCreationDto, Photo>();
            //  CreateMap<UserForRegisterDto, User>();
            CreateMap<MessageForCreationDto, Message>().ReverseMap();
            CreateMap<Message, MessageToReturnDto>()
                .ForMember(m => m.SenderPhotoUrl, opt =>
                opt.MapFrom(u => u.Sender.Photos.FirstOrDefault(p => p.IsMain).Url))
                      .ForMember(m => m.RecipientPhotoUrl, opt =>
                opt.MapFrom(u => u.Recipient.Photos.FirstOrDefault(p => p.IsMain).Url));  //Individual Mapping

        }
    }
}
