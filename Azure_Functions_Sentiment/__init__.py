import logging

import azure.functions as func
from pattern.text.en import sentiment


def main(req: func.HttpRequest) -> func.HttpResponse:
    logging.info('Python HTTP trigger function processed a request.')

    # Parse the input text
    text = req.params.get('text')
    if not text:
        try:
            req_body = req.get_json()
        except ValueError:
            pass
        else:
            text = req_body.get('text')

    if text:
        # Analyze sentiment
        sentiment_value = sentiment(text)
        logging.info(f"Sentiment analysis result: {sentiment_value}")

        # Return the sentiment value
        return func.HttpResponse(
            f"The sentiment of the text is: {sentiment_value}",
            status_code=200
        )
    else:
        return func.HttpResponse(
            "Please pass a text on the query string or in the request body",
            status_code=400
        )