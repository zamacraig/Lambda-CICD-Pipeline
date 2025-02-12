import yfinance as yf
import pandas as pd
from yahoo_fin.stock_info import get_data

def lambda_handler(event, context):

    pd.set_option('display.max_columns', None)

    startDate="12/12/2019"
    endDate="12/12/2019"
    interval = "1d"
    ticker_list =["API", "TSLA", "GOOG", "LMT"]

    tradevolume_df = pd.DataFrame(columns=['open', 'high', 'low', 'close', 'adjclose', 'volume', 'ticker'])

    historical_datas = {}
    for ticker in ticker_list:
        historical_datas[ticker] = get_data(ticker, start_date= None, end_date= None, index_as_date = True, interval=interval)
        tradevolume_df = pd.concat([tradevolume_df, historical_datas[ticker]], ignore_index=True)

    return { 'statusCode': 200, 'body': 'Success' }