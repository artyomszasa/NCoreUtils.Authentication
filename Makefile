
NAME=ncoreutils-oauth2
VERSION?=1.0.13

build-image:
	docker build -t gcr.io/hosting-666/$(NAME):$(VERSION) .

push-image: build-image
	gcloud docker -- push gcr.io/hosting-666/$(NAME):$(VERSION)