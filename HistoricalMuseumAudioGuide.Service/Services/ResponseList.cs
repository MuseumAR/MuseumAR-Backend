using System.Collections.Generic;
using System.Linq;

namespace HistoricalMuseumAudioGuide.Service.Services
{
    public class ResponseList
    {
        public static ResponseModel AggregateResponses(IEnumerable<ResponseModel> responses, string message)
        {
            var index = 1;
            foreach (var response in responses)
            {
                response.Id = index;
                index++;
            }

            var firstError = responses.FirstOrDefault(r => r.StatusCode != 200);
            if (firstError != null) return firstError;

            return ResponseModel.Success(message, responses.ToList());
        }
    }
}
