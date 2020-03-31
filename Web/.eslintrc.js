module.exports = {
    'env': {
        'browser': true,
        'es6': true,
        'jquery': true
    },
    'extends': [
        'eslint:recommended',
        'plugin:vue/essential'
    ],
    'globals': {
        'Atomics': 'readonly',
        'DevExpress': 'readonly',
        'd3': 'writable',
        'moment': 'readonly',
        'partial': 'readonly',
        'root': 'writable',
        'signalR': 'readonly',
        'SharedArrayBuffer': 'readonly',
        'Vue': 'readonly'
    },
    'parserOptions': {
        'ecmaVersion': 2018,
        'sourceType': 'module'
    },
    'plugins': [
        'vue'
    ],
    'rules': {
        'brace-style': [
            'error',
            'allman',
            { 'allowSingleLine': true }
        ],
        'indent': [
            'error',
            4,
            { 'SwitchCase': 1 }
        ],
        'linebreak-style': [
            'error',
            'windows'
        ],
        'no-console': 'off',
        'no-unused-vars': 'off',
        'quotes': [
            'error',
            'single'
        ],
        'semi': [
            'error',
            'always'
        ]
    }
};