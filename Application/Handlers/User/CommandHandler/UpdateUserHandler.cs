//using Application.Commands.Customer;
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
//    public class UpdateUserHandler : IRequestHandler<UpdateUserCommand>
//    {
//        private readonly IUserService _userService;

//        public UpdateUserHandler(IUserService userService)
//        {
//            _userService = userService;
//        }
//        public async Task Handle(UpdateUserCommand request, CancellationToken cancellationToken)
//        {
//            await _userService.Update(request.User);
//        }
//    }

//}
