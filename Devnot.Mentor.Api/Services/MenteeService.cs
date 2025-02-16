﻿using AutoMapper;
using DevnotMentor.Api.Entities;
using DevnotMentor.Api.Enums;
using DevnotMentor.Api.Helpers.Extensions;
using DevnotMentor.Api.Repositories.Interfaces;
using DevnotMentor.Api.Services.Interfaces;
using System;
using System.Threading.Tasks;
using DevnotMentor.Api.Common;
using DevnotMentor.Api.Common.Response;
using DevnotMentor.Api.Configuration.Context;
using DevnotMentor.Api.CustomEntities.Dto;
using DevnotMentor.Api.CustomEntities.Request.MenteeRequest;
using System.Collections.Generic;
using DevnotMentor.Api.CustomEntities.Request.CommonRequest;

namespace DevnotMentor.Api.Services
{
    public class MenteeService : BaseService, IMenteeService
    {
        private readonly IMenteeRepository menteeRepository;
        private readonly IMenteeLinksRepository menteeLinksRepository;
        private readonly IMenteeTagsRepository menteeTagsRepository;
        private readonly ITagRepository tagRepository;
        private readonly IUserRepository userRepository;
        private readonly IMentorRepository mentorRepository;
        private readonly IMentorApplicationsRepository applicationsRepository;
        private readonly IMentorMenteePairsRepository pairsRepository;

        public MenteeService(
            IMapper mapper,
            IMenteeRepository menteeRepository,
            IMenteeLinksRepository menteeLinksRepository,
            IMenteeTagsRepository menteeTagsRepository,
            ITagRepository tagRepository,
            IUserRepository userRepository,
            IMentorRepository mentorRepository,
            IMentorApplicationsRepository mentorApplicationsRepository,
            IMentorMenteePairsRepository mentorMenteePairsRepository,
            ILoggerRepository loggerRepository,
            IDevnotConfigurationContext devnotConfigurationContext
            )
            : base(mapper, loggerRepository, devnotConfigurationContext)
        {
            this.menteeRepository = menteeRepository;
            this.menteeLinksRepository = menteeLinksRepository;
            this.menteeTagsRepository = menteeTagsRepository;
            this.tagRepository = tagRepository;
            this.userRepository = userRepository;
            this.mentorRepository = mentorRepository;
            this.applicationsRepository = mentorApplicationsRepository;
            this.pairsRepository = mentorMenteePairsRepository;
        }

        public async Task<ApiResponse<MenteeDto>> GetMenteeProfileAsync(string userName)
        {
            var mentee = await menteeRepository.GetByUserNameAsync(userName);

            if (mentee == null)
            {
                return new ErrorApiResponse<MenteeDto>(ResponseStatus.NotFound, data: default, message: ResultMessage.NotFoundMentee);
            }

            var mappedMentee = mapper.Map<Mentee, MenteeDto>(mentee);
            return new SuccessApiResponse<MenteeDto>(mappedMentee);
        }

        public async Task<ApiResponse<List<MentorDto>>> GetPairedMentorsByUserIdAsync(int userId)
        {
            var mentee = await menteeRepository.GetByUserIdAsync(userId);

            if (mentee == null)
            {
                return new ErrorApiResponse<List<MentorDto>>(ResponseStatus.NotFound, data: default, message: ResultMessage.NotFoundMentee);
            }

            var pairedMentors = mapper.Map<List<MentorDto>>(await menteeRepository.GetPairedMentorsByMenteeIdAsync(mentee.Id));
            return new SuccessApiResponse<List<MentorDto>>(pairedMentors);
        }

        public async Task<ApiResponse<MenteeDto>> CreateMenteeProfileAsync(CreateMenteeProfileRequest request)
        {
            var user = await userRepository.GetByIdAsync(request.UserId);

            if (user == null)
            {
                return new ErrorApiResponse<MenteeDto>(ResponseStatus.NotFound, data: default, message: ResultMessage.NotFoundUser);
            }

            var registeredMentee = await menteeRepository.GetByUserIdAsync(user.Id);

            if (registeredMentee != null)
            {
                return new ErrorApiResponse<MenteeDto>(data: default, message: ResultMessage.MenteeAlreadyRegistered);
            }

            var mentee = CreateNewMentee(request, user);

            if (mentee == null)
            {
                return new ErrorApiResponse<MenteeDto>(data: default, ResultMessage.FailedToAddMentee);
            }

            var mappedMentee = mapper.Map<MenteeDto>(mentee);
            return new SuccessApiResponse<MenteeDto>(mappedMentee);
        }

        private Mentee CreateNewMentee(CreateMenteeProfileRequest request, User user)
        {
            Mentee mentee = null;

            var newMentee = mapper.Map<Mentee>(request);
            newMentee.UserId = user.Id;

            mentee = menteeRepository.Create(newMentee);

            if (mentee == null)
            {
                return null;
            }

            menteeLinksRepository.Create(mentee.Id, request.MenteeLinks);

            foreach (var menteeTag in request.MenteeTags)
            {
                if (String.IsNullOrWhiteSpace(menteeTag))
                {
                    continue;
                }

                var tag = tagRepository.Get(menteeTag);

                if (tag != null)
                {
                    menteeTagsRepository.Create(new MenteeTags { TagId = tag.Id, MenteeId = mentee.Id });
                }
                else
                {
                    var newTag = tagRepository.Create(new Tag { Name = menteeTag });

                    if (newTag != null)
                    {
                        menteeTagsRepository.Create(new MenteeTags { TagId = newTag.Id, MenteeId = mentee.Id });
                    }
                }
            }

            return mentee;
        }
        
        public async Task<ApiResponse<List<MenteeDto>>> SearchAsync(SearchRequest request)
        {
            var mappedMentees = mapper.Map<List<MenteeDto>>(await menteeRepository.SearchAsync(request));
            return new SuccessApiResponse<List<MenteeDto>>(mappedMentees);
        }
    }
}
