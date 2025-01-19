using iText.StyledXmlParser.Jsoup.Nodes;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Queries.Order
{
    public record class GetAllOrdersQuery:IRequest<IEnumerable<Domain.Entities.Order>>;
    
}
