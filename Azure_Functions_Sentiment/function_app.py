import azure.functions as func
import logging
import json
from pattern.text.en import sentiment

app = func.FunctionApp(http_auth_level=func.AuthLevel.FUNCTION)

@app.route(route="Azure_Functions_Sentiment")
def Azure_Functions_Sentiment(req: func.HttpRequest) -> func.HttpResponse:
    logging.info('Python HTTP trigger function processed a request.')

    text = req.params.get('text')
    if not text:
        try:
            req_body = req.get_json()
        except ValueError:
            pass
        else:
            text = req_body.get('text')

    if text:
        sentiment_value = sentiment(text)
        logging.info(f"Sentiment analysis result: {sentiment_value}")

        sentiment_response = {
            "polarity": sentiment_value[0],
            "subjectivity": sentiment_value[1]
        }

        return func.HttpResponse(
            json.dumps(sentiment_response),
            status_code=200,
            headers={"Content-Type": "application/json"}
        )
    else:
        return func.HttpResponse(
            "Please pass a text on the query string or in the request body",
            status_code=400
        )

