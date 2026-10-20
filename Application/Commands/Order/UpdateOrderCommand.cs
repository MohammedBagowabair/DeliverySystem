using MediatR;

namespace Application.Commands.Order
{
    public record UpdateOrderCommand(Domain.Entities.Order Order, bool AllowClosedDayEdit = false) : IRequest;
}
