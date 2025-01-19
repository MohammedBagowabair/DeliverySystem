//using Application.Commands.User;
//using Application.Interfaces;
//using MediatR;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Application.Handlers.User.CommandHandler
//{
//    public class CreateUserHandler(IUserService _userService) : IRequestHandler<CreateUserCommand, Domain.Entities.User>
//    {
//        public async Task<Domain.Entities.User> Handle(CreateUserCommand request, CancellationToken cancellationToken)
//        {
//            return await _userService.Create(request.User);
//        }
//    }
//}
