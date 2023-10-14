FROM python:3.11

RUN pip install --upgrade pip
RUN apt-get update && apt-get install -y \
    g++ \
    gcc \
    libffi-dev \
    openssl

RUN useradd -m appuser
USER appuser
WORKDIR /home/appuser

COPY --chown=appuser:appuser requirements.txt requirements.txt

ENV PATH="/home/appuser/.local/bin:${PATH}"
RUN pip install --user -r requirements.txt

COPY --chown=appuser:appuser . .
RUN python download_data.py
CMD [ "gunicorn", "-b" , "0.0.0.0:5001", "app:app"]